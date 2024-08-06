namespace Bfs.TestTask.Driver;

public class CardDriverTests
{
    private readonly ICardDriverMock _cardDriverMock = new CardDriverMock();

    [Fact]
    public async Task ReadCard_SetCardData_ReturnCardData()
    {
        var readCardTask = _cardDriverMock.ReadCard(CancellationToken.None);
        _cardDriverMock.SetCardData(new CardData("1234 1234 1234 1234"));

        var result = await readCardTask;

        Assert.NotNull(result);
        Assert.Equal("1234 1234 1234 1234", result.CardNumber);
    }

    [Fact]
    public async Task ReadCard_CancelTask_ReturnNull()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var readCardTask = _cardDriverMock.ReadCard(cancellationTokenSource.Token);

        await cancellationTokenSource.CancelAsync();

        var result = await readCardTask;

        Assert.Null(result);
    }

    [Fact]
    public async Task ReadCard_CantReadCard_ReturnNull()
    {
        var readCardTask = _cardDriverMock.ReadCard(CancellationToken.None);
        _cardDriverMock.CantReadCard();

        var result = await readCardTask;

        Assert.Null(result);
    }

    [Fact]
    public async Task EjectCard_TakeCard_ReturnCardTaken()
    {
        var readCardTask = _cardDriverMock.EjectCard(CancellationToken.None);

        await using var enumerator = readCardTask.GetAsyncEnumerator();
        var first = await enumerator.MoveNextAsync() ? enumerator.Current : default;
            
        Assert.Equal(EjectResult.Ejected, first);
            
        _cardDriverMock.TakeCard();
            
        var second = await enumerator.MoveNextAsync() ? enumerator.Current : default;
            
        Assert.Equal(EjectResult.CardTaken, second);
    }
    
    [Fact]
    public async Task EjectCard_CancelTask_ReturnRetracted()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var readCardTask = _cardDriverMock.EjectCard(cancellationTokenSource.Token);

        await using var enumerator = readCardTask.GetAsyncEnumerator(CancellationToken.None);
        var first = await enumerator.MoveNextAsync() ? enumerator.Current : default;
            
        Assert.Equal(EjectResult.Ejected, first);
            
        await cancellationTokenSource.CancelAsync();
            
        var second = await enumerator.MoveNextAsync() ? enumerator.Current : default;
            
        Assert.Equal(EjectResult.Retracted, second);
    }
}