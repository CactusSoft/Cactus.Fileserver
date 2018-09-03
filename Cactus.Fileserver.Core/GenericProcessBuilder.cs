using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Core
{
    public class GenericPipelineBuilder<TMeta> where TMeta : IFileInfo
    {
        private readonly
            IList<Func<Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>, Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>>> addProcessors =
                new List<Func<Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>,
                    Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>>>();

        private readonly IList<Func<Func<HttpRequest, IFileGetContext<TMeta>, Task>, Func<HttpRequest, IFileGetContext<TMeta>, Task>>> getProcessors = new List<Func<Func<HttpRequest, IFileGetContext<TMeta>, Task>, Func<HttpRequest, IFileGetContext<TMeta>, Task>>>();

        public GenericPipelineBuilder<TMeta> Use(
            Func<Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>, Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>>
                processor)
        {
            addProcessors.Add(processor);
            return this;
        }

        public GenericPipelineBuilder<TMeta> Use(Func<Func<HttpRequest, IFileGetContext<TMeta>, Task>, Func<HttpRequest, IFileGetContext<TMeta>, Task>>
                processor)
        {
            getProcessors.Add(processor);
            return this;
        }

        //Run for "add" pipeline
        public Func<HttpRequest, HttpContent, TMeta, Task<TMeta>> Run(
            Func<HttpRequest, HttpContent, TMeta, Task<TMeta>> finalizer)
        {
            if (addProcessors.Count == 0)
                return finalizer;

            return addProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }
}