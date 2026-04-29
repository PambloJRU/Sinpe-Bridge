package com.gruposinpe.proyectosinpe_kotlin.receiver

import com.gruposinpe.proyectosinpe_kotlin.model.SmsRequest

interface SmsReceiverCallback {
    fun onSmsReceived(smsRequest: SmsRequest)
}