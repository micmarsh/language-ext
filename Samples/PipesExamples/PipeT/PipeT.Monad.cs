using LanguageExt.Common;
using LanguageExt.Traits;

namespace LanguageExt.Pipes2;

public class PipeT<IN, OUT, M> : MonadT<PipeT<IN, OUT, M>, M>
    where M : Monad<M>
{
    static K<PipeT<IN, OUT, M>, B> Monad<PipeT<IN, OUT, M>>.Bind<A, B>(
        K<PipeT<IN, OUT, M>, A> ma, 
        Func<A, K<PipeT<IN, OUT, M>, B>> f) => 
        ma.As().Bind(x => f(x).As());

    static K<PipeT<IN, OUT, M>, B> Functor<PipeT<IN, OUT, M>>.Map<A, B>(
        Func<A, B> f, 
        K<PipeT<IN, OUT, M>, A> ma) => 
        ma.As().Map(f);

    static K<PipeT<IN, OUT, M>, A> Applicative<PipeT<IN, OUT, M>>.Pure<A>(A value) => 
        PipeT.pure<IN, OUT, M, A>(value);

    static K<PipeT<IN, OUT, M>, B> Applicative<PipeT<IN, OUT, M>>.Apply<A, B>(
        K<PipeT<IN, OUT, M>, Func<A, B>> mf,
        K<PipeT<IN, OUT, M>, A> ma) =>
        ma.As().ApplyBack(mf.As());

    static K<PipeT<IN, OUT, M>, B> Applicative<PipeT<IN, OUT, M>>.Action<A, B>(
        K<PipeT<IN, OUT, M>, A> ma, 
        K<PipeT<IN, OUT, M>, B> mb) =>
        PipeT.liftM<IN, OUT, M, B>(ma.As().Run().Action(mb.As().Run()));

    static K<PipeT<IN, OUT, M>, A> Applicative<PipeT<IN, OUT, M>>.Actions<A>(IEnumerable<K<PipeT<IN, OUT, M>, A>> fas)
    {
        K<M, A>? ma = null;
        foreach (var fa in fas)
        {
            switch (ma)
            {
                case null:
                    ma = fa.As().Run();
                    break;
                
                default:
                    ma = ma.Action(fa.As().Run());
                    break;
            }
        }
        return ma is null
            ? throw Errors.SequenceEmpty
            : PipeT.liftM<IN, OUT, M, A>(ma);
    }

    static K<PipeT<IN, OUT, M>, A> Applicative<PipeT<IN, OUT, M>>.Actions<A>(IAsyncEnumerable<K<PipeT<IN, OUT, M>, A>> fas)
    {
        return PipeT.liftM<IN, OUT, M, A>(go(fas));
            
        static async ValueTask<K<M, A>> go(IAsyncEnumerable<K<PipeT<IN, OUT, M>, A>> fas)
        {
            K<M, A>? ma = null;

            await foreach (var fa in fas)
            {
                switch (ma)
                {
                    case null:
                        ma = await fa.As().RunAsync();
                        break;

                    default:
                        ma = ma.Action(await fa.As().RunAsync());
                        break;
                }
            }
            return ma ?? throw Errors.SequenceEmpty;
        }
    }

    static K<PipeT<IN, OUT, M>, A> MonadT<PipeT<IN, OUT, M>, M>.Lift<A>(K<M, A> ma) => 
        PipeT.liftM<IN, OUT, M, A>(ma);

    static K<PipeT<IN, OUT, M>, A> MonadIO<PipeT<IN, OUT, M>>.LiftIO<A>(IO<A> ma) => 
        PipeT.liftIO<IN, OUT, M, A>(ma);

    static K<PipeT<IN, OUT, M>, B> MonadIO<PipeT<IN, OUT, M>>.MapIO<A, B>(K<PipeT<IN, OUT, M>, A> ma, Func<IO<A>, IO<B>> f) => 
        ma.As().MapIO(f);

    static K<PipeT<IN, OUT, M>, IO<A>> MonadIO<PipeT<IN, OUT, M>>.ToIO<A>(K<PipeT<IN, OUT, M>, A> ma) => 
        ma.MapIO(IO.pure);
}
