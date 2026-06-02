package com.gruposinpe.proyectosinpe_kotlin.model

data class PhoneHeartbeatRequest(
    val deviceId: String,
    val sentAtUtc: String
)
