using MediatR;
using System.Runtime.Serialization;

namespace eShop.Ordering.API.Application.Commands;

[DataContract]
public class CompleteOrderCommand : IRequest<bool>
{
    [DataMember]
    public int OrderNumber { get; private set; }

    public CompleteOrderCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}