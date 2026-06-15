'use client';

import InputTemp from '../../Components/Forms/InputTemp';
import ButtonTemp from '../../Components/Forms/ButtonTemp';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { apiFetch } from '@/lib/api';

export default function RegisterPage() {
  const router = useRouter();
  const [errorMessage, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await apiFetch('/auth/register', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      router.push('/');
      router.refresh();
    } catch (error: any) {
      console.log('API error:', error);
      setError(error?.message || 'Nie prawidlowe dane');
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className='flex flex-col gap-4 justify-center items-center'
    >
      <h1 className='font-bold text-purple-900 text-2xl'>Zarejestruj się</h1>
      <InputTemp
        type='text'
        name='username'
        placeholder='Nazwa uzytkownika'
        label='Nazwa uzytkownika'
      />
      <InputTemp
        type='email'
        name='email'
        placeholder='Email'
        label='Adres e-mail'
      />
      <InputTemp
        type='password'
        name='password'
        placeholder='Hasło'
        label='Hasło'
      />

      {errorMessage ? <p className='text-red-600'>{errorMessage}</p> : null}

      <ButtonTemp buttonText='Zarejestruj się' />
    </form>
  );
}
