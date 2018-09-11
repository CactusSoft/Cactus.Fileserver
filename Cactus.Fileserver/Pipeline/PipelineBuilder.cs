using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Pipeline
{
    public delegate Task<MetaInfo> FileProcessorDelegate(HttpRequest request, HttpContent content, Stream stream, IFileInfo info);

    public delegate FileProcessorDelegate PipelineDelegate(FileProcessorDelegate next);

    public class PipelineBuilder
    {
        private readonly IList<PipelineDelegate> _processors = new List<PipelineDelegate>();
        private FileProcessorDelegate _entrance;
        
        public PipelineBuilder Use(PipelineDelegate processor)
        {
            if (_entrance != null)
                throw new ArgumentException(
                    "The pipeline has already finalized with Run(FileProcessorDelegate finalizer)");

            _processors.Add(processor);
            return this;
        }

        //Run for "add" pipeline
        public FileProcessorDelegate Run(FileProcessorDelegate finalizer)
        {
            _entrance = _processors.Count == 0 ? finalizer : _processors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));

            return _entrance;
        }
    }
}