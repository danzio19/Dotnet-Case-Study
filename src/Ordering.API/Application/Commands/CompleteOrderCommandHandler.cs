using MediatR;
using Microsoft.Extensions.Logging;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure.Idempotency;
using eShop.Ordering.API.Application.IntegrationEvents;

namespace eShop.Ordering.API.Application.Commands;


public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CompleteOrderCommandHandler> _logger;

    public CompleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<CompleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<bool> Handle(CompleteOrderCommand message, CancellationToken cancellationToken)
    {
        var orderToUpdate = await _orderRepository.GetAsync(message.OrderNumber);
        
        if (orderToUpdate is null)
        {
            return false;
        }

        orderToUpdate.SetCompletedStatus();
        
        _orderRepository.Update(orderToUpdate);

        _logger.LogInformation("----- Completing Order - Order: {@Order}", orderToUpdate);

        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}