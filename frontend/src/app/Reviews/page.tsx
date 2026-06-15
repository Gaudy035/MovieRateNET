import Movie from '@/Interfaces/Movie';
import Review from '@/Interfaces/Review';
import { apiFetch } from '@/lib/api';
import RateBtn from '@/Components/MovieBlock/subcomponents/RateBtn';
import { cookies } from 'next/headers';
import ReviewBlock from '@/Components/ReviewBlock/ReviewBlock';

export default async function ReviewsPage({
  searchParams,
}: {
  searchParams: Promise<{ movie_id?: number }>;
}) {
  const { movie_id } = await searchParams;
  const movie: Movie = await apiFetch(`/movies/${movie_id}`);
  const avgRating = movie.average_rating;
  const reviews: Review[] = await apiFetch(`/reviews/${movie_id}`);

  const cookieStorage = await cookies();
  const isLoggedIn = cookieStorage.has('access_token');

  return (
    <div className='flex flex-col justify-center items-center py-8 px-16 gap-16 w-full'>
      <div className='grid grid-cols-3 gap-12'>
        <img
          src={movie.poster_url}
          alt={`Plakat${movie.title}`}
          className='rounded-xl w-full h-auto'
        />
        <div className='col-span-2 py-8 flex flex-col justify-start items-start gap-2'>
          <h1 className='font-bold text-3xl'>{movie.title}</h1>
          <p className='font-semibold text-2xl'>
            {avgRating
              ? `Średnia ocen: ${Number(avgRating).toFixed(1)} / 10`
              : 'Brak recenzji'}
          </p>
          <p className='font-semibold text-xl'>
            Czas Trwania: {movie.duration}
          </p>
          <p className='font-semibold text-xl'>Opis:</p>
          <p className='text-lg text-left'>{movie.description}</p>
        </div>
      </div>
      <div className='flex justify-between items-center w-full px-10'>
        <h2 className='font-semibold text-3xl'>Opinie</h2>
        <RateBtn movieId={movie_id!} isLoggedIn={isLoggedIn} />
      </div>
      {/* Opinie */}
      <div className='flex flex-col justify-center items-center gap-8 w-full'>
        {reviews.length > 0 ? (
          reviews.map((review) => (
            <ReviewBlock
              key={review.review_id}
              username={review.username}
              title={review.title}
              body={review.body}
              rating={review.rating}
              created_at={review.created_at}
            />
          ))
        ) : (
          <p className='text-3xl'>Brak opinii</p>
        )}
      </div>
    </div>
  );
}
