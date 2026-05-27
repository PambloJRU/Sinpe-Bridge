namespace ProyectoIngenieriaBACKEND_POS.Models.Enums
{
    /// <summary>
    /// Indica qué tan grave o urgente es un evento registrado en el log.
    /// El administrador puede ordenar o filtrar la bitácora por este campo
    /// para atender primero los eventos más críticos.
    /// </summary>
    public enum RiskLevel
    {
        /// <summary>
        /// Operación normal, sin problemas.
        /// Ejemplo: pago confirmado exitosamente.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Situación que requiere atención pero no es fraude confirmado.
        /// Ejemplo: pago recibido sin orden activa, pago vencido por pocos minutos.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Posible fraude o error grave detectado.
        /// Ejemplo: referencia duplicada, dispositivo no autorizado.
        /// </summary>
        High = 3,

        /// <summary>
        /// El sistema está comprometido o hay una falla que detiene operaciones.
        /// Ejemplo: teléfono receptor desconectado por más de 15 minutos.
        /// </summary>
        Critical = 4
    }
}
