using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;


        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var result = await _orderService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderCreateDTO dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                await _orderService.AssociateOrderWithPayment(order.Id); //luego de crear la orden, buscamos un pago durante 5 minutos
                return Ok("orden creada y asociada");
            }
            catch (TimeoutException ex)
            {
                return BadRequest("Pago no recibido dentro del tiempo límite");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
