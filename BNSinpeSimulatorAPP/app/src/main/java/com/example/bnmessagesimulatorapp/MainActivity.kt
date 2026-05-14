package com.example.bnmessagesimulatorapp

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Bundle
import android.telephony.SmsManager
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import android.os.Build
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                    SinpeSimulatorApp()
                }
            }
        }
    }
}

@Composable
fun SinpeSimulatorApp() {
    // Estados para guardar los valores de los inputs
    var de by remember { mutableStateOf("Nombre cualquiera") }
    var numeroDestino by remember { mutableStateOf("60405995") } // Por defecto el número que espera tu receiver
    var monto by remember { mutableStateOf("10000") }
    val context = LocalContext.current

    // solicitar el permiso en tiempo de ejecución
    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            enviarSmsSinpe(context, de, numeroDestino, monto)
        } else {
            Toast.makeText(context, "Se requiere permiso para enviar SMS", Toast.LENGTH_SHORT).show()
        }
    }

    // Interfaz de Usuario (UI)
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Text("Simulador SMS SINPE", style = MaterialTheme.typography.headlineSmall)

        Spacer(modifier = Modifier.height(24.dp))

        OutlinedTextField(
            value = de,
            onValueChange = { de = it },
            label = { Text("Número Destino") },
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Text),
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(modifier = Modifier.height(24.dp))

        OutlinedTextField(
            value = numeroDestino,
            onValueChange = { numeroDestino = it },
            label = { Text("Número Destino") },
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(modifier = Modifier.height(16.dp))

        OutlinedTextField(
            value = monto,
            onValueChange = { monto = it },
            label = { Text("Monto (Colones)") },
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(modifier = Modifier.height(32.dp))

        Button(
            onClick = {
                // tenemos el permiso?
                if (ContextCompat.checkSelfPermission(context, Manifest.permission.SEND_SMS) == PackageManager.PERMISSION_GRANTED) {
                    enviarSmsSinpe(context, de , numeroDestino, monto)
                } else {
                    // Si no, lo pedimos
                    permissionLauncher.launch(Manifest.permission.SEND_SMS)
                }
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Enviar SMS de Prueba")
        }
    }
}

fun enviarSmsSinpe(context: Context, de: String, numeroDestino: String, monto: String) {
    try {
        // prefijo de fecha y hora actual (yyyyMMddHHmm)
        val sdf = SimpleDateFormat("yyyyMMddHHmmss", Locale.getDefault())
        val fechaHoraPrefix = sdf.format(Date())

        //  sufijo aleatorio de 11 dígitos.
        //  String.format con "%011d" para asegurar que siempre tenga 11 caracteres,
        // rellenando con ceros a la izquierda si el número generado es más corto
        val sufijoAleatorio = String.format(Locale.getDefault(), "%011d", (0..99999999999L).random())

        //  crear la referencia final de 23 dígitos
        val refSimulada = "$fechaHoraPrefix$sufijoAleatorio"

        //   mensaje final
        val mensajeSinpe = "SINPE Movil: Ha recibido una transferencia de $de por $monto colones. Ref: $refSimulada."

        //  versión de Android para obtener el SmsManager correctamente
        val smsManager: SmsManager = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            context.getSystemService(SmsManager::class.java)
        } else {
            @Suppress("DEPRECATION")
            SmsManager.getDefault()
        }

        // Enviamos el SMS
        smsManager.sendTextMessage(numeroDestino, null, mensajeSinpe, null, null)

        Toast.makeText(context, "SMS Enviado - Ref: $refSimulada", Toast.LENGTH_LONG).show()
    } catch (e: Exception) {
        Toast.makeText(context, "Error: ${e.message}", Toast.LENGTH_LONG).show()
    }
}