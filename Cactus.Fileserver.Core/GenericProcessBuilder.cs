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

        private readonly IList<Func<Func<T, Task<Stream>>, Func<T, Task<Stream>>>> getProcessors = new List<Func<Func<T, Task<Stream>>, Func<T, Task<Stream>>>>();

        public GenericPipelineBuilder<T> Use(
            Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>
                processor)
        {
            addProcessors.Add(processor);
            return this;
        }

        public GenericPipelineBuilder<T> Use(Func<Func<T, Task<Stream>>, Func<T, Task<Stream>>>
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

        public Func<T, Task<Stream>> Run(
            Func<T, Task<Stream>> finalizer)
        {
            if (getProcessors.Count == 0)
                return finalizer;

            return getProcessors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }
}