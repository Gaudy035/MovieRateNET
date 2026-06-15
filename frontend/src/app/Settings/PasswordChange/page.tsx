'use client';
import { apiFetch } from '@/lib/api';
import InputTemp from '@/Components/Forms/InputTemp';
import ButtonTemp from '@/Components/Forms/ButtonTemp';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

export default function PasswordChangePage() {
  const router = useRouter();
  const [errorMessage, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);
  const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    const formData = new FormData(e.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await apiFetch('/auth/change-password', {
        method: 'PATCH',
        body: JSON.stringify(payload),
      });

      setSuccess(true);

      setTimeout(() => {
        router.push('/Login');
        router.refresh();
      }, 2000);
    } catch (error: any) {
      console.log('API error', error);
      setError(error?.message);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className='flex flex-col gap-4 justify-center items-center'
    >
      <h1 className='font-bold text-purple-900 text-2xl'>Zmień hasło</h1>
      <InputTemp
        type='password'
        name='new_password'
        placeholder='Nowe hasło'
        label='Nowe hasło'
      />
      <InputTemp
        type='password'
        name='password'
        placeholder='Aktualne hasło'
        label='Aktualne hasło'
      />
      {errorMessage ? <p className='text-red-600'>{errorMessage}</p> : null}
      {success ? <p className='text-green-600'>Zmiana hasła udana!</p> : null}
      <ButtonTemp buttonText='Zapisz' />
    </form>
  );
}
