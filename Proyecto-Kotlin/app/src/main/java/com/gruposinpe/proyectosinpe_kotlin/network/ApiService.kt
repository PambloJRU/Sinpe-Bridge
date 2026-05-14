package com.gruposinpe.proyectosinpe_kotlin.network
import retrofit2.Response
import com.gruposinpe.proyectosinpe_kotlin.model.SmsRequest
import okhttp3.ResponseBody
import retrofit2.http.Body
import retrofit2.http.POST

// TODO: Implementar cliente HTTP para enviar pagos al backend .NET

interface ApiService {

    @POST("api/Sms/receive")
    suspend fun sendRealSMSToServer(@Body sms: SmsRequest): Response<ResponseBody>




}