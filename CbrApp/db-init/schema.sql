-- Инициализация базы для докерской PostgreSQL 15

SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;

-- Таблица currencies
CREATE TABLE public.currencies (
    num_code character varying(10) NOT NULL,
    char_code character(3) NOT NULL,
    name character varying(100) NOT NULL,
    CONSTRAINT currencies_pkey PRIMARY KEY (num_code),
    CONSTRAINT unique_char_code UNIQUE (char_code)
);

-- Таблица exchange_rates
CREATE TABLE public.exchange_rates (
    currency_num_code character varying(10) NOT NULL,
    date date NOT NULL,
    nominal integer NOT NULL CHECK (nominal > 0),
    value numeric(18,4) NOT NULL CHECK (value > 0),
    CONSTRAINT exchange_rates_pkey PRIMARY KEY (date, currency_num_code),
    CONSTRAINT exchange_rates_currency_num_code_fkey FOREIGN KEY (currency_num_code)
        REFERENCES public.currencies (num_code)
);

-- Таблица, в которой фиксируются даты, для которых уже была произведена загрузка курсов
CREATE TABLE public.processed_dates (
    date date PRIMARY KEY,
    status text
);

-- Индексы
CREATE INDEX idx_exchange_rates_currency ON public.exchange_rates (currency_num_code);
CREATE INDEX idx_exchange_rates_date ON public.exchange_rates (date);
CREATE INDEX idx_exchange_rates_date_desc ON public.exchange_rates (date DESC);

-- View
CREATE VIEW public.exchange_rates_sorted AS
SELECT currency_num_code,
       date,
       nominal,
       value
FROM public.exchange_rates
ORDER BY date DESC;
