using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServices.Domain.Models
{
    public class MessageModel
    {
        public string Content { get; set; }
        public string CorrelationId { get; set; }
        public string ReplyTo { get; set; }
    }
}
