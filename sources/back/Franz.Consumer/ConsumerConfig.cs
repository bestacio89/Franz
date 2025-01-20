using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Franz.Consumer;
public class KafkaConsumerConfig
{
  public string BootstrapServers { get; set; }
  public string GroupId { get; set; }
  public bool EnableAutoCommit { get; set; } = false;
  public string AutoOffsetReset { get; set; } = "Earliest";
}
