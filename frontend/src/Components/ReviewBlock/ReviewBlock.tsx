import Rating from '../MovieBlock/subcomponents/Rating';

interface ReviewBlockProps {
  username: string;
  title?: string;
  body?: string;
  rating: number;
  created_at: Date;
}

export default function ReviewBlock({
  username,
  title,
  body,
  rating,
  created_at,
}: ReviewBlockProps) {
  const dateString = created_at.toString();
  const [y, m, d] = dateString.slice(0, 10).split('-');
  const date = `${d}-${m}-${y}`;
  return (
    <article className='flex flex-col gap-2 w-3/5 px-6 py-4 border-2 border-purple-900 rounded-2xl'>
      <div className='grid grid-cols-3 w-full'>
        <p className='font-semibold text-lg'>{username}</p>
        <Rating ratingScore={rating} />
        <p className='flex justify-end font-semibold'>{date}</p>
      </div>
      <p className='text-xl font-semibold'>{title}</p>
      <p>{body}</p>
    </article>
  );
}
