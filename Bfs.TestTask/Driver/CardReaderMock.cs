using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Bfs.TestTask.Driver;

public class CardDriverMock : ICardDriverMock
{
    private CardData? _cardData;
    private bool _canReadCard = true;
    private readonly Channel<EjectResult> _ejectChannel = Channel.CreateUnbounded<EjectResult>();

    public void SetCardData(CardData cardData)
    {
        _cardData = cardData;
    }

    public void CantReadCard()
    {
        _canReadCard = false;
    }

    public void TakeCard()
    {
        _ejectChannel.Writer.TryWrite(EjectResult.CardTaken);
    }

    public async Task<CardData?> ReadCard(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _canReadCard)
        {
            await Task.Delay(10);
            return _cardData;
        }

        return null;
    }

    public async IAsyncEnumerable<EjectResult> EjectCard(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await _ejectChannel.Writer.WriteAsync(EjectResult.Ejected);

        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await _ejectChannel.Reader.ReadAsync(cancellationToken);
        }

        yield return EjectResult.Retracted;
    }
}
