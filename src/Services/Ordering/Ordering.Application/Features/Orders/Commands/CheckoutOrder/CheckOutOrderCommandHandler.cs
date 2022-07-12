using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;

namespace Ordering.Application.Features.Orders.Commands.CheckoutOrder;

public class CheckOutOrderCommandHandler : IRequestHandler<CheckOutOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<CheckOutOrderCommandHandler> _logger;

    public CheckOutOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IEmailService emailService, ILogger<CheckOutOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(orderRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> Handle(CheckOutOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = _mapper.Map<Order>(request);

        var newOrder = await _orderRepository.AddAsync(orderEntity);

        _logger.LogInformation($"New order with Order Id: {newOrder.Id} is created");

        await SendEmail(newOrder);

        return newOrder.Id;
    }

    private async Task SendEmail(Order order)
    {
        var email = new Email() { To = "chhotumal.ingole@gmail.com", Subject = "Order was created", Body = $"Order was created." };

        try
        {
            await _emailService.SendEmail(email);
        }
        catch(Exception ex)
        {
            _logger.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
        }

    }
}
