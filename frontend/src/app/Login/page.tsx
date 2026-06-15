'use client';

import Link from 'next/link';
import InputTemp from '../../Components/Forms/InputTemp';
import ButtonTemp from '../../Components/Forms/ButtonTemp';
import { apiFetch } from '@/lib/api';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

export default function LoginPage() {
  const router = useRouter();
  const [errorMessage, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await apiFetch('/auth/login', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      router.push('/');
      router.refresh();
    } catch (error: any) {
      console.log('API error', error);
      setError(error?.message || 'Nieprawidlowy email lub haslo');
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className='flex flex-col gap-4 justify-center items-center'
    >
      <h1 className='font-bold text-purple-900 text-2xl'>Zaloguj się</h1>
      <InputTemp type='email' name='email' placeholder='Email' label='E-mail' />
      <InputTemp
        type='password'
        name='password'
        placeholder='Haslo'
        label='Haslo'
      />

      {errorMessage ? <p className='text-red-600'>{errorMessage}</p> : null}

      <ButtonTemp buttonText='Zaloguj się' />

      <div className='flex flex-col justify-center items-center'>
        <span>Nie masz konta?</span>
        <Link href={'/Register'} className='underline text-purple-900'>
          Zarejestruj się!
        </Link>
      </div>
    </form>
  );
}
