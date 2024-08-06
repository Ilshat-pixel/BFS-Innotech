using System.Text;
using System.Threading.Channels;

namespace Bfs.TestTask.Parser;

public class Parser : IParser
{
    public async IAsyncEnumerable<IMessage> Parse(ChannelReader<ReadOnlyMemory<byte>> source)
    {
        List<byte> buffer = new List<byte>();

        while (await source.WaitToReadAsync())
        {
            while (source.TryRead(out var data))
            {
                buffer.AddRange(data.ToArray());

                while (buffer.Count >= 2)
                {
                    // Read the length bytes (2 bytes)
                    int length = (buffer[0] << 8) | buffer[1];

                    if (buffer.Count < length + 2)
                    {
                        // Not enough data yet, wait for more data
                        break;
                    }

                    // Read the full message based on the length
                    byte[] messageBytes = buffer.GetRange(2, length).ToArray();
                    buffer.RemoveRange(0, length + 2);

                    string message = Encoding.ASCII.GetString(messageBytes);

                    // Identify and parse the message
                    IMessage parsedMessage = ParseMessage(message);
                    if (parsedMessage != null)
                    {
                        yield return parsedMessage;
                    }
                }
            }
        }
    }

    private IMessage ParseMessage(string message)
    {
        var parts = message.Split(new char[] { '', '' }, StringSplitOptions.None);
        if (parts.Length < 2)
        {
            return null; // invalid message format
        }

        var messageClass = parts[0][0];
        var messageSubClass = parts[0][1];

        switch (messageClass)
        {
            case '1':
                if (messageSubClass == '2')
                {
                    return ParseCardReaderState(parts);
                }
                break;
            case '2':
                if (messageSubClass == '2')
                {
                    if (parts[3] == "B")
                    {
                        return ParseSendStatus(parts);
                    }
                    else if (parts[3] == "F")
                    {
                        return ParseGetFitnessData(parts);
                    }
                }
                break;
        }

        return null;
    }

    private CardReaderState ParseCardReaderState(string[] parts)
    {
        var luno = parts[1];
        var dig = parts[3][0];
        var deviceStatus = int.Parse(parts[3][1].ToString());
        var errorSeverity = int.Parse(parts[3][2].ToString());
        var diagnosticStatus = int.Parse(parts[3][3].ToString());
        var suppliesStatus = int.Parse(parts[3][4].ToString());

        return new CardReaderState(
            luno,
            dig,
            deviceStatus,
            errorSeverity,
            diagnosticStatus,
            suppliesStatus
        );
    }

    private SendStatus ParseSendStatus(string[] parts)
    {
        var luno = parts[1];
        var statusDescriptor = parts[3][0];
        var transactionNumber = int.Parse(parts[5]);

        return new SendStatus(luno, statusDescriptor, transactionNumber);
    }

    private GetFitnessData ParseGetFitnessData(string[] parts)
    {
        var luno = parts[1];
        var statusDescriptor = parts[3][0];
        var messageIdentifier = parts[4][0];
        var hardwareFitnessIdentifier = parts[4][1];
        var fitnessStates = new List<FitnessState>()
        {
            new FitnessState(parts[4][2], parts[4].Substring(3))
        };

        for (int i = 5; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length >= 2)
            {
                var dig = part[0];
                var fitness = part.Substring(1);
                fitnessStates.Add(new FitnessState(dig, fitness));
            }
        }

        return new GetFitnessData(
            luno,
            statusDescriptor,
            messageIdentifier,
            hardwareFitnessIdentifier,
            fitnessStates.ToArray()
        );
    }
}
