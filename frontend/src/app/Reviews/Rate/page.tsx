import { apiFetch } from '@/lib/api';
import ReviewForm from '@/Components/Forms/ReviewForm';
import Movie from '@/Interfaces/Movie';

export default async function ReviewFormPage({
  searchParams,
}: {
  searchParams: Promise<{ movie_id: number }>;
}) {
  const { movie_id } = await searchParams;
  const movieData: Movie = await apiFetch(`/movies/${movie_id}`);
  const movieTitle = movieData.title;

  return <ReviewForm movieTitle={movieTitle} movieId={movie_id} />;
}
