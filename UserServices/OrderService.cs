using AutoMapper;
using DTO_s;
using Entities;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IMapper mapper,
            IProductService productService,
            IKafkaProducerService kafkaProducerService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _productService = productService;
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
        }

        public async Task<OrderDTO> GetById(int id)
        {
            Order order = await _orderRepository.GetOrderById(id);
            OrderDTO orderDTO = _mapper.Map<Order, OrderDTO>(order);
            return orderDTO;
        }

        public async Task<OrderDTO> AddOrder(OrderDTO order)
        {
            Order ord = _mapper.Map<OrderDTO, Order>(order);
            int checkedSum = await checkOrderSum(ord.OrderItems);
            if (checkedSum != order.OrderSum)
            {
                ord.OrderSum = checkedSum;
                _logger.LogInformation($"Order number {order.OrderId} with incorrect sum, the order sum is {checkedSum}");
            }

            Order res = await _orderRepository.AddOrder(ord);

            TransactionMessage transactionMessage = new(
                TransactionId: Guid.NewGuid().ToString("N"),
                UserId: res.UserId,
                OrderId: res.OrderId,
                Amount: res.OrderSum ?? 0,
                Currency: "ILS",
                Status: "Created",
                CreatedAtUtc: DateTime.UtcNow);

            try
            {
                await _kafkaProducerService.SendTransactionAsync(transactionMessage);
                _logger.LogInformation("Published order transaction event for order {OrderId}", res.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order transaction event for order {OrderId}", res.OrderId);
            }

            OrderDTO orderDTO = _mapper.Map<Order, OrderDTO>(res);
            return orderDTO;
        }

        private async Task<int> checkOrderSum(ICollection<OrderItem> orderItems)
        {
            int sum = 0;
            foreach (var item in orderItems)
            {
                ProductDTO product = await _productService.GetProductById(item.ProductId);
                sum += (int)(item.Quantity * product.Price);
            }
            return sum;
        }
    }
}
