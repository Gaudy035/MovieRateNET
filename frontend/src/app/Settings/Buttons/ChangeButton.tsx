import { useRouter } from 'next/navigation';
import Link from 'next/link';

interface ChangeButtonProps {
  url: string;
  text: string;
}

export default function ChangeButton({ url, text }: ChangeButtonProps) {
  const router = useRouter();
  return (
    <Link
      href={url}
      className='cursor-pointer hover:scale-105 transition duration-200'
    >
      {text}
    </Link>
  );
}
