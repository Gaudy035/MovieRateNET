interface ButtonTempProps {
  buttonText: string;
}

export default function ButtonTemp({ buttonText }: ButtonTempProps) {
  return (
    <button
      type='submit'
      className='bg-purple-900 text-white rounded-xl px-3 py-2 cursor-pointer hover:scale-105 duration-200'
    >
      {buttonText}
    </button>
  );
}
