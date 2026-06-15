'use client';

interface ReviewRateBtnProps {
  btnText: number;
  btnValue: number;
  active?: boolean;
  onClick: () => void;
}

export default function ReviewRateBtn({
  btnText,
  btnValue,
  active,
  onClick,
}: ReviewRateBtnProps) {
  return (
    <button
      type='button'
      className={
        active
          ? 'border-2 font-semibold text-white bg-purple-900 border-purple-900 cursor-pointer rounded-full aspect-square w-8 h-8 transition duration-200'
          : 'border-2 font-semibold border-purple-900 text-purple-900 bg-white cursor-pointer rounded-full aspect-square w-8 h-8 hover:text-white hover:bg-purple-900 hover:scale-110 transition duration-200'
      }
      onClick={onClick}
      value={btnValue}
    >
      {btnText}
    </button>
  );
}
