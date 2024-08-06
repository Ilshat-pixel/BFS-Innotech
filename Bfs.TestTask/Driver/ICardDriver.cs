namespace Bfs.TestTask.Driver;

public interface ICardDriver
{
    Task<CardData?> ReadCard(CancellationToken cancellationToken);
    
    IAsyncEnumerable<EjectResult> EjectCard(CancellationToken cancellationToken);
}

public interface ICardDriverMock : ICardDriver
{
    void SetCardData(CardData cardData);
    void CantReadCard();
    void TakeCard();
}

public enum EjectResult
{
    Ejected,
    CardTaken,
    Retracted
}

public record CardData(string CardNumber);