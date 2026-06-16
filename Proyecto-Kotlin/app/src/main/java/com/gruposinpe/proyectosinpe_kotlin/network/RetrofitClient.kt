package com.gruposinpe.proyectosinpe_kotlin.network

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import com.gruposinpe.proyectosinpe_kotlin.BuildConfig
import okhttp3.OkHttpClient
import java.util.concurrent.TimeUnit

object RetrofitClient {
    // IMPORTANTE: la IP esta en el archivo local.properties, modifiquenla cada uno como les convenga

    private val BASE_URL = BuildConfig.BASE_URL+"/"

    val okHttpClient = OkHttpClient.Builder()
        .connectTimeout(20, TimeUnit.SECONDS) // Aumentado a 30 seg
        .readTimeout(20, TimeUnit.SECONDS)    // Aumentado a 30 seg
        .writeTimeout(20, TimeUnit.SECONDS)
        .build()

    val instance: ApiService by lazy {
        val retrofit = Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        retrofit.create(ApiService::class.java)
    }
}