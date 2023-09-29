using System;
using System.Threading;
using ROS2;

namespace ConsoleApplication
{
    public class RCLDotnetTalker
    {
        public static void Main(string[] args)
        {
            RCLdotnet.Init();

            Node node = RCLdotnet.CreateNode("talker");

            node.DeclareParameter("publish_string_prefix", "Hello World");

            Publisher<std_msgs.msg.String> chatterPub = node.CreatePublisher<std_msgs.msg.String>("chatter");

            std_msgs.msg.String msg = new std_msgs.msg.String();

            int i = 1;

            while (RCLdotnet.Ok())
            {
                msg.Data = $"{node.GetParameter("publish_string_prefix").StringValue}: {i}";
                i++;
                Console.WriteLine($"Publishing: \"{msg.Data}");
                chatterPub.Publish(msg);

                RCLdotnet.SpinOnce(node, 100L);

                // Sleep a little bit between each message
                Thread.Sleep(1000);
            }
        }
    }
}
