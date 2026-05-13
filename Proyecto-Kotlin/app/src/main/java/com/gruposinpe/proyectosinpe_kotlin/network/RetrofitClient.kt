package com.gruposinpe.proyectosinpe_kotlin.network

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object RetrofitClient {
    // IMPORTANTE: Si van a usar el emulador de Android, usa 10.0.2.2
    // Si van a usar el teléfono físico, usen la IP de la compu (tienen que buscarla si, ej: 192.168.1.15)

    private const val BASE_URL = "http://192.168.50.150:5198/"

    val instance: ApiService by lazy {
        val retrofit = Retrofit.Builder()
            .baseUrl(BASE_URL)
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        retrofit.create(ApiService::class.java)
    }
}