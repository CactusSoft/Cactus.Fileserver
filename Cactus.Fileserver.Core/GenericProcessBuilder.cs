using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public class GenericPipelineBuilder<T>
    {
        private readonly
            IList<Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>
            > addProcessors =
                new List<Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>,
                    Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>>();

        private readonly IList<Func<Func<T, Stream, Task>, Func<T, Stream, Task>>> getProcessors = new List<Func<Func<T, Stream, Task>, Func<T, Stream, Task>>>();

        public GenericPipelineBuilder<T> Use(
            Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>
                processor)
        {
            addProcessors.Add(processor);
            return this;
        }

        public GenericPipelineBuilder<T> Use(Func<Func<T, Stream, Task>, Func<T, Stream, Task>>
                processor)
        {
            getProcessors.Add(processor);
            return this;
        }

        public Func<T, HttpContent, IFileInfo, Task<MetaInfo>> Run(
            Func<T, HttpContent, IFileInfo, Task<MetaInfo>> finalizer)
        {
            if (addProcessors.Count == 0)
                return finalizer;

            return addProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }

        public Func<T, Stream, Task> Run(
            Func<T, Stream, Task> finalizer)
        {
            if (getProcessors.Count == 0)
                return finalizer;

            return getProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }
}