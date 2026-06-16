package com.gruposinpe.proyectosinpe_kotlin

import android.Manifest
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.provider.Settings
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.systemBarsPadding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.material3.Divider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import com.gruposinpe.proyectosinpe_kotlin.model.PhoneHeartbeatRequest
import com.gruposinpe.proyectosinpe_kotlin.model.SmsRequest
import com.gruposinpe.proyectosinpe_kotlin.network.RetrofitClient
import com.gruposinpe.proyectosinpe_kotlin.receiver.SmsReceiver
import com.gruposinpe.proyectosinpe_kotlin.receiver.SmsReceiverCallback
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import java.time.Instant
import java.time.LocalTime
import java.time.format.DateTimeFormatter

enum class LogType { HEARTBEAT, SMS_RECEIVED, SMS_SENT, ERROR, INFO }

data class LogEntry(
    val timestamp: String,
    val type: LogType,
    val message: String
)

class MainActivity : ComponentActivity(), SmsReceiverCallback {
    private val heartbeatIntervalMs = 30000L
    private val deviceId by lazy {
        Settings.Secure.getString(contentResolver, Settings.Secure.ANDROID_ID) ?: "unknown"
    }

    private val _logs = mutableStateListOf<LogEntry>()
    private val _smsReceivedCount = mutableIntStateOf(0)
    private val _smsSentOkCount = mutableIntStateOf(0)
    private val _errorCount = mutableIntStateOf(0)

    private fun addLog(type: LogType, message: String) {
        val timestamp = LocalTime.now().format(DateTimeFormatter.ofPattern("HH:mm:ss"))
        _logs.add(0, LogEntry(timestamp, type, message))
        if (_logs.size > 100) _logs.removeAt(_logs.lastIndex)

        when (type) {
            LogType.SMS_RECEIVED -> _smsReceivedCount.intValue++
            LogType.SMS_SENT, LogType.INFO -> _smsSentOkCount.intValue++
            LogType.ERROR -> _errorCount.intValue++
            else -> {}
        }

        Log.d("SINPE_BRIDGE", message)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        SmsReceiver.callback = this
        requestSmsPermission()
        startHeartbeat()

        enableEdgeToEdge()
        setContent {
            MaterialTheme {
                LogScreen(
                    logs = _logs,
                    smsReceivedCount = _smsReceivedCount.intValue,
                    smsSentOkCount = _smsSentOkCount.intValue,
                    errorCount = _errorCount.intValue
                )
            }
        }
    }

    private fun startHeartbeat() {
        lifecycleScope.launch {
            repeatOnLifecycle(Lifecycle.State.STARTED) {
                while (isActive) {
                    sendHeartbeat()
                    delay(heartbeatIntervalMs)
                }
            }
        }
    }

    private suspend fun sendHeartbeat() {
        val heartbeat = PhoneHeartbeatRequest(
            deviceId = deviceId,
            sentAtUtc = Instant.now().toString()
        )

        try {
            val response = RetrofitClient.instance.sendHeartbeat(heartbeat)
            if (response.isSuccessful) {
                addLog(LogType.HEARTBEAT, "Heartbeat OK")
            } else {
                addLog(LogType.ERROR, "Heartbeat error: ${response.code()}")
            }
        } catch (e: Exception) {
            addLog(LogType.ERROR, "Heartbeat error: ${e.message}")
        }
    }

    private fun requestSmsPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            if (ContextCompat.checkSelfPermission(
                    this, Manifest.permission.RECEIVE_SMS
                ) != PackageManager.PERMISSION_GRANTED
            ) {
                ActivityCompat.requestPermissions(
                    this, arrayOf(Manifest.permission.RECEIVE_SMS), 100
                )
            }
        }
    }

    override fun onSmsReceived(smsRequest: SmsRequest) {
        val phone = if (smsRequest.senderNumber.length > 8) {
            smsRequest.senderNumber.takeLast(8)
        } else {
            smsRequest.senderNumber
        }
        addLog(LogType.SMS_RECEIVED, "SMS de $phone")
        sendRealSMSCycleScope(smsRequest)
    }

    private fun sendRealSMSCycleScope(smsRequest: SmsRequest) {
        lifecycleScope.launch {
            addLog(LogType.SMS_SENT, "Enviando al server...")
            try {
                val response = RetrofitClient.instance.sendRealSMSToServer(smsRequest)
                if (response.isSuccessful) {
                    addLog(LogType.INFO, "Enviado exitoso" )
                } else {
                    addLog(LogType.ERROR, "Error servidor: ${response.code()}")
                }
            } catch (e: Exception) {
                addLog(LogType.ERROR, "Error conexion: ${e.message}")
            }
        }
    }
}

@Composable
fun LogScreen(
    logs: List<LogEntry>,
    smsReceivedCount: Int,
    smsSentOkCount: Int,
    errorCount: Int
) {
    val listState = rememberLazyListState()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF1A1A2E))
            .systemBarsPadding()
            .padding(16.dp)
    ) {
        Text(
            text = "SINPE Bridge",
            fontSize = 22.sp,
            fontWeight = FontWeight.Bold,
            color = Color.White
        )
        Spacer(modifier = Modifier.height(4.dp))
        Text(
            text = "Conectado al servidor",
            fontSize = 14.sp,
            color = Color(0xFF4CAF50)
        )

        Spacer(modifier = Modifier.height(12.dp))
        Divider(color = Color(0xFF333355))
        Spacer(modifier = Modifier.height(8.dp))

        if (logs.isEmpty()) {
            Column(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth(),
                verticalArrangement = Arrangement.Center,
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text(
                    text = "Esperando actividad...",
                    color = Color(0xFF666688),
                    fontSize = 14.sp
                )
            }
        } else {
            LazyColumn(
                state = listState,
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
            ) {
                items(logs) { entry ->
                    LogItem(entry)
                }
            }
        }

        Spacer(modifier = Modifier.height(8.dp))
        Divider(color = Color(0xFF333355))
        Spacer(modifier = Modifier.height(8.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Text(
                text = "SMS recibidos: $smsReceivedCount",
                color = Color(0xFF9999BB),
                fontSize = 12.sp
            )
            Text(
                text = "OK: $smsSentOkCount | Errores: $errorCount",
                color = Color(0xFF9999BB),
                fontSize = 12.sp
            )
        }
    }
}

@Composable
fun LogItem(entry: LogEntry) {
    val (prefix, textColor) = when (entry.type) {
        LogType.HEARTBEAT -> Pair("[OK] ", Color(0xFF117515))
        LogType.SMS_RECEIVED -> Pair("[SMS] ", Color(0xFF2196F3))
        LogType.SMS_SENT -> Pair("[>>] ", Color(0xFF9E9E9E))
        LogType.ERROR -> Pair("[!!] ", Color(0xFFF44336))
        LogType.INFO -> Pair("[OK] ", Color(0xFF8BC34A))
    }

    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 2.dp)
    ) {
        Text(
            text = entry.timestamp,
            color = Color(0xFF666688),
            fontSize = 12.sp,
            fontFamily = FontFamily.Monospace
        )
        Spacer(modifier = Modifier.padding(horizontal = 4.dp))
        Text(
            text = prefix,
            color = textColor,
            fontSize = 12.sp,
            fontFamily = FontFamily.Monospace,
            fontWeight = FontWeight.Bold
        )
        Text(
            text = entry.message,
            color = textColor,
            fontSize = 12.sp,
            fontFamily = FontFamily.Monospace
        )
    }
}
