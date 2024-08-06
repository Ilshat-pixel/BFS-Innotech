namespace Bfs.TestTask.Parser;

public class ParserTest
{
    [Fact]
    public async Task ParseMessages()
    {
        var cardReaderState = new CardReaderState("00100355", 'D', 1, 0, 0, 1);
        var sendStatus = new SendStatus("00100355", 'B', 4321);
        var getFitnessData = new GetFitnessData("00100355", 'F', 'J', 'A', new FitnessState[]
        {
            new('D', "01"),
            new('y', "1"),
            new('A', "0"),
            new('E', "00000"),
            new('G', "0"),
            new('L', "0"),
            new('w', "00040003000200010"),
            new('H', "0")
        });
        
        var source = new MessageSource();

        var channel = source.Reader;

        var parser = new Parser();

        source.StartConsume();

        int index = 0;

        await foreach (var message in parser.Parse(channel))
        {
            if (message is CardReaderState cardReaderStateMessage)
            {
                Assert.Equal(0, index);
                Assert.Equal(cardReaderState, cardReaderStateMessage);
            }
            else if (message is SendStatus sendStatusMessage)
            {
                Assert.Equal(1, index);
                Assert.Equal(sendStatus, sendStatusMessage);
            }
            else if (message is GetFitnessData getFitnessDataMessage)
            {
                Assert.Equal(2, index);
                Assert.Equivalent(getFitnessData, getFitnessDataMessage, true);
            }

            index++;
        }

        Assert.Equal(3, index);
    }
}