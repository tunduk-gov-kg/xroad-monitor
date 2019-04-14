using AutoMapper;
using Catalog.Domain.Entity;
using Catalog.Domain.Helpers;

namespace Catalog.Domain.Mapping.Resolvers
{
    public class ProducerMessageLifecycleResolver : IValueResolver<OperationalDataRecord, Message, MessageLifecycle>
    {
        public MessageLifecycle Resolve(OperationalDataRecord source, Message destination, MessageLifecycle destMember,
            ResolutionContext context)
        {
            if (source.IsProducer)
            {
                return new MessageLifecycle
                {
                    RequestInTs = source.RequestInTs?.AsMilliSecondsToDateTime(),
                    RequestOutTs = source.RequestOutTs?.AsMilliSecondsToDateTime(),
                    ResponseInTs = source.ResponseInTs?.AsMilliSecondsToDateTime(),
                    ResponseOutTs = source.ResponseOutTs?.AsMilliSecondsToDateTime()
                };
            }

            return new MessageLifecycle();
        }
    }
}