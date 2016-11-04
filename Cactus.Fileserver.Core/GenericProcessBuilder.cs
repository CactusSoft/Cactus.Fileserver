using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public class GenericPipelineBuilder<T>
    {
        private readonly IList<Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>> processors = new List<Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>>>();

        public GenericPipelineBuilder<T> Use(Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>, Func<T, HttpContent, IFileInfo, Task<MetaInfo>>> processor)
        {
            processors.Add(processor);
            return this;
        }

        public Func<T, HttpContent, IFileInfo, Task<MetaInfo>> Run(Func<T, HttpContent, IFileInfo, Task<MetaInfo>> finalizer)
        {
            if (processors.Count == 0)
                return finalizer;

            return processors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }
}
