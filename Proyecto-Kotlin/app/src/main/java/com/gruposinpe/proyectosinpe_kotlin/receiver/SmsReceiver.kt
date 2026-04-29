package com.gruposinpe.proyectosinpe_kotlin.receiver

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.os.Build
import android.os.Bundle
import android.telephony.SmsMessage
import android.util.Log
import android.widget.Toast
import com.gruposinpe.proyectosinpe_kotlin.model.SmsRequest
import java.time.Instant
import java.time.LocalTime
import java.time.format.DateTimeFormatter

class SmsReceiver : BroadcastReceiver() {

    companion object{
        private const val TAG = "SmsReceiver"
        var callback: SmsReceiverCallback? = null
    }

    override fun onReceive(context: Context?, intent: Intent?) {
        if (intent?.action == "android.provider.Telephony.SMS_RECEIVED"){
            val bundle: Bundle? = intent.extras

            if(bundle != null){
                try{
                    val pdus = bundle.get("pdus") as Array<*>

                    for(pdu in pdus) {
                        val smsMessage: SmsMessage =
                            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                                val format = bundle.getString("format")
                                SmsMessage.createFromPdu(pdu as ByteArray, format)
                            } else {
                                SmsMessage.createFromPdu(pdu as ByteArray)
                            }

                        val sender = smsMessage.originatingAddress ?: "Desconocido"
                        val message = smsMessage.messageBody ?: ""
                        val timestamp = smsMessage.timestampMillis

                        val receivedAt = convertTimestampToISO(timestamp)

                        Log.d(TAG, "Origen: $sender")
                        Log.d(TAG, "Mensaje: $message")

                        val normalizeSender = sender.replace("-", "").replace(" ", "")

                        if(normalizeSender.endsWith("60405995")){
                            Log.d(TAG, "Mensaje del Banco nacional recibido")

                            if (context != null) {
                                Toast.makeText(context, "SMS BNCR Recibido", Toast.LENGTH_SHORT).show()
                            }
                        }

                        val smsRequest = SmsRequest(
                            senderNumber = sender,
                            messageBody = message,
                            receivedAt = receivedAt
                        )

                        //aca se envía el sms al mainActivity
                        callback?.onSmsReceived(smsRequest)
                    }
                } catch(e: Exception){
                    Log.e(TAG, "Error: ${e.message}")
                }
            }
        }
    }

    private fun convertTimestampToISO(timestamp: Long): String {
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val instant = Instant.ofEpochMilli(timestamp)
            DateTimeFormatter.ISO_INSTANT.format(instant)
        } else {
            java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'").apply {
                timeZone = java.util.TimeZone.getTimeZone("UTC")
            }.format(java.util.Date(timestamp))
        }
    }
}
