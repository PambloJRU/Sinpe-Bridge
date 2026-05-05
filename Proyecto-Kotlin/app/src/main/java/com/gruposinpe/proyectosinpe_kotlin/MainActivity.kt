package com.gruposinpe.proyectosinpe_kotlin

import android.Manifest
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.gruposinpe.proyectosinpe_kotlin.ui.theme.ProyectoSinpeKotlinTheme

import retrofit2.Response
import okhttp3.ResponseBody
import androidx.lifecycle.lifecycleScope
import kotlinx.coroutines.launch
import android.util.Log
import android.widget.Toast
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.material3.MaterialTheme
import androidx.compose.ui.unit.dp
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.gruposinpe.proyectosinpe_kotlin.model.SmsRequest
import com.gruposinpe.proyectosinpe_kotlin.network.RetrofitClient
import com.gruposinpe.proyectosinpe_kotlin.receiver.SmsReceiver
import com.gruposinpe.proyectosinpe_kotlin.receiver.SmsReceiverCallback


class MainActivity : ComponentActivity(), SmsReceiverCallback {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        //Avisa cuando llega un mensaje
        SmsReceiver.callback = this

        requestSmsPermission()

        enableEdgeToEdge()
        setContent {
            Column(
                modifier = Modifier.fillMaxSize(),
                verticalArrangement = Arrangement.Center,
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text(text = "SINPE Bridge - Estado: Activo")

                Button(onClick = {
                    // Aquí llamamos a la lógica que creamos para la Tarea 02
                    enviarPruebaAlServidor()
                }) {
                    Text("Simular Envío de SINPE")
                }
            }
        }
    }

    //Pedir permisos para leer SMS
    private fun requestSmsPermission(){
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.M){
            if(ContextCompat.checkSelfPermission(
                this, Manifest.permission.RECEIVE_SMS
            ) != PackageManager.PERMISSION_GRANTED
            ){
                ActivityCompat.requestPermissions(
                    this, arrayOf(Manifest.permission.RECEIVE_SMS), 100
                )
            }
        }

    }

    //Acá se recibe el sms real, ya construido
    override fun onSmsReceived(smsRequest: SmsRequest) {
        Log.d("SMS_LLEGÓ", "SMS RECIBIDO: $smsRequest")
        //acá llamar al metodo para enviar el sms
        enviarSmsAlServidor(smsRequest)
    }

    //Con este metodo enviamos el SMS real al back
    private fun enviarSmsAlServidor(smsRequest: SmsRequest) {
        lifecycleScope.launch {
            try {
                val response = RetrofitClient.instance.enviarSmsAlServidor(smsRequest)

                if (response.isSuccessful) {
                    Log.d("SINPE_BRIDGE", "¡SMS REAL enviado a .NET!")
                } else {
                    Log.e("SINPE_BRIDGE", "Error: ${response.code()}")
                }
            } catch (e: Exception) {
                Log.e("SINPE_BRIDGE", "Error de conexión: ${e.message}")
            }
        }
    }

    private fun enviarPruebaAlServidor() {
        // 1. Creamos un objeto de prueba con el formato que espera tu C#
        val smsPrueba = SmsRequest(
            senderNumber = "2627-3342", // Número típico de notificaciones BCCR
            messageBody = "SINPE Movil: Ha recibido una transferencia de PABLO RAMIREZ por 1500 colones. Ref: 20245110123456789012345.",
            receivedAt = "2026-04-23T10:30:00Z" // Formato ISO para DateTime de .NET
        )

        //CON EL MENSAJE, CONSTRUIR EL "smsPrueba" aqui

        // 2. Ejecutamos la petición en una corrutina (hilo secundario)
        lifecycleScope.launch {
            try {
                val response = RetrofitClient.instance.enviarSmsAlServidor(smsPrueba)

                if (response.isSuccessful) {
                    Log.d("SINPE_BRIDGE", " ¡CONECTADO! El servidor respondió: ${response.body()?.string()}")
                } else {
                    Log.e("SINPE_BRIDGE", " Error del servidor: Código ${response.code()}")
                }
            } catch (e: Exception) {
                // Aquí capturamos si la IP está mal o el servidor está apagado
                Log.e("SINPE_BRIDGE", " Error de conexión: ${e.message}")
            }
        }
    }

    @Composable
    fun MainScreen() {
        Column(
            modifier = Modifier.fillMaxSize(),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(text = "SINPE Bridge - Panel de Pruebas", style = MaterialTheme.typography.headlineMedium)
            Spacer(modifier = Modifier.height(20.dp))

            Button(onClick = { enviarPruebaAlServidor() }) {
                Text("Probar conexión con .NET")
            }
        }
    }


}



