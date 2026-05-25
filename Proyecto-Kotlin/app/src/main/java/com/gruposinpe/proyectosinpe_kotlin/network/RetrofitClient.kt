package com.gruposinpe.proyectosinpe_kotlin.network

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import com.gruposinpe.proyectosinpe_kotlin.BuildConfig
object RetrofitClient {
    // IMPORTANTE: la IP esta en el archivo local.properties, modifiquenla cada uno como les convenga

    private val BASE_URL = BuildConfig.BASE_URL

    val instance: ApiService by lazy {
        val retrofit = Retrofit.Builder()
            .baseUrl(BASE_URL)
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        retrofit.create(ApiService::class.java)
    }
}