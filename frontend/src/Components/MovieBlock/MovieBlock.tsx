import Rating from './subcomponents/Rating';
import RateBtn from './subcomponents/RateBtn';
import ViewReviews from './subcomponents/ViewReviews';

interface MovieBlockProps {
  movieId: string | number;
  imgUrl: string;
  movieTitle: string;
  movieDesc: string;
  averageRating: number | null;
  isLoggedIn: boolean;
}

export default function MovieBlock({
  movieId,
  imgUrl,
  movieTitle,
  movieDesc,
  averageRating,
  isLoggedIn,
}: MovieBlockProps) {
  return (
    <article className='grid grid-cols-4 max-w-4xl gap-8 py-1.5 mx-6'>
      <div className='p2 flex justify-center items-center'>
        <img src={imgUrl} alt='Plakat' className='rounded-xl' />
      </div>
      <div className='col-span-3 flex flex-col items-start gap-4 py-4'>
        <h2 className='font-bold text-2xl'>{movieTitle}</h2>
        <Rating
          ratingScore={averageRating ? Number(averageRating).toFixed(1) : null}
        />
        <p className='h-24 line-clamp-5'>{movieDesc}</p>
        <div className='flex flex-row gap-4'>
          <RateBtn movieId={movieId} isLoggedIn={isLoggedIn} />
          <ViewReviews movieId={movieId} />
        </div>
      </div>
    </article>
  );
}
