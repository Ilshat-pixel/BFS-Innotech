using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Bfs.TestTask.Driver;

public class CardDriverMock : ICardDriverMock
{
    private CardData? _cardData;
    private bool _canReadCard = true;
    private readonly ConcurrentQueue<EjectResult> _ejectResults =
        new ConcurrentQueue<EjectResult>();
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
        _ejectResults.Enqueue(EjectResult.CardTaken);
        _ejectChannel.Writer.TryWrite(EjectResult.CardTaken);
    }

    public async Task<CardData?> ReadCard(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        try
        {
            await Task.Delay(10, cancellationToken); // Simulate delay
            return _cardData;
        }
        catch (OperationCanceledException _)
        {
            return null;
        }
    }

    public async IAsyncEnumerable<EjectResult> EjectCard(
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        _ejectChannel.Writer.TryWrite(EjectResult.Ejected);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_ejectChannel.Reader.TryRead(out var result))
            {
                yield return result;

                if (result == EjectResult.CardTaken)
                {
                    yield break;
                }
            }
        }

        yield return EjectResult.Retracted;
    }
}
