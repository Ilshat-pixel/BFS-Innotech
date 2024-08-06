using System.Text;
using System.Threading.Channels;

namespace Bfs.TestTask.Parser;

public class MessageSource
{
    private readonly Channel<ReadOnlyMemory<byte>> _channel;

    public MessageSource()
    {
        _channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();
    }

    public ChannelReader<ReadOnlyMemory<byte>> Reader => _channel.Reader;

    public async Task StartConsume()
    {
        var cardReaderState = @"1200100355D1001";
        var sendStatus = @"2200100355B4321";
        var fitnessData = @"2200100355FJAD01y1A0E00000G0L0w00040003000200010H0";

        await WriteMessage(cardReaderState);
        await Task.Delay(100);
        await WriteMessage(sendStatus);
        await Task.Delay(100);
        await WriteMessage(fitnessData);
        await Task.Delay(100);
        _channel.Writer.Complete();
    }

    private async Task WriteMessage(string message)
    {
        var length = message.Length + 2;
        var array = (new byte[length]);
        Encoding.ASCII.GetBytes(message, array.AsSpan().Slice(2));
        array[0] = (byte)(message.Length / 256);
        array[1] = (byte)(message.Length % 256);

        var arrayMemory = array.AsMemory();

        var firstPart = Random.Shared.Next(2, length);
        await _channel.Writer.WriteAsync(arrayMemory.Slice(0, firstPart));

        int writed = firstPart;
        while (writed < length)
        {
            var partLength = Random.Shared.Next(1, length - writed);
            await _channel.Writer.WriteAsync(arrayMemory.Slice(writed, partLength));
            writed += partLength;
        }
    }
}