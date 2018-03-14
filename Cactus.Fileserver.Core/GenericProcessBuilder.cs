using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public class GenericPipelineBuilder<TRequest,TMeta> where TMeta : IFileInfo
    {
        private readonly
            IList<Func<Func<TRequest, HttpContent, TMeta, Task<TMeta>>, Func<TRequest, HttpContent, TMeta, Task<TMeta>>>
            > addProcessors =
                new List<Func<Func<TRequest, HttpContent, TMeta, Task<TMeta>>,
                    Func<TRequest, HttpContent, TMeta, Task<TMeta>>>>();

        private readonly IList<Func<Func<TRequest, IFileGetContext<TMeta>, Task>, Func<TRequest, IFileGetContext<TMeta>, Task>>> getProcessors = new List<Func<Func<TRequest, IFileGetContext<TMeta>, Task>, Func<TRequest, IFileGetContext<TMeta>, Task>>>();

        public GenericPipelineBuilder<TRequest, TMeta> Use(
            Func<Func<TRequest, HttpContent, TMeta, Task<TMeta>>, Func<TRequest, HttpContent, TMeta, Task<TMeta>>>
                processor)
        {
            addProcessors.Add(processor);
            return this;
        }

        public GenericPipelineBuilder<TRequest, TMeta> Use(Func<Func<TRequest, IFileGetContext<TMeta>, Task>, Func<TRequest, IFileGetContext<TMeta>, Task>>
                processor)
        {
            getProcessors.Add(processor);
            return this;
        }

        //Run for "add" pipeline
        public Func<TRequest, HttpContent, TMeta, Task<TMeta>> Run(
            Func<TRequest, HttpContent, TMeta, Task<TMeta>> finalizer)
        {
            if (addProcessors.Count == 0)
                return finalizer;

            return addProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }

        public Func<TRequest, IFileGetContext<TMeta>, Task> Run(
            Func<TRequest, IFileGetContext<TMeta>, Task> finalizer)
        {
            if (getProcessors.Count == 0)
                return finalizer;

            return getProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }
}