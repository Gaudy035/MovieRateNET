'use client';

import { useRouter } from 'next/navigation';
import { apiFetch } from '@/lib/api';

export default function LogoutButton() {
  const router = useRouter();
  const hanleLogout = async () => {
    try {
      await apiFetch('/auth/logout', { method: 'POST' });
      router.push('/');
      router.refresh();
    } catch (error) {
      console.log('Blad przy wylogowywaniu', error);
    }
  };
  return (
    <button
      onClick={() => hanleLogout()}
      className='text-red-600 cursor-pointer hover:scale-105 transition duration-200'
    >
      Wyloguj się
    </button>
  );
}
