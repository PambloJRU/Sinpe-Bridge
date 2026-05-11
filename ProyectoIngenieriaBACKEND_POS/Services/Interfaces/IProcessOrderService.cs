using ProyectoIngenieriaBACKEND_POS.Models.Entities;

namespace ProyectoIngenieriaBACKEND_POS.Services.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// si alguien lee esto pipi la idea es crear una nueva orden con estado PENDIENTE en la base de datos.
        /// </summary>
        /// <param name="orderDto">Los datos de la orden enviados desde el POS (React).</param>
        /// <returns>La entidad Order creada con su Id asignado.</returns>
        Task<Order> CreateOrderAsync(OrderCreateDTO orderDto);
    }
}
