create table FaresPairs
(
    Timestamp            timestamp default CURRENT_TIMESTAMP null comment 'Represents date time when record was created',
    Origin               varchar(3)                          not null comment 'IATA airport code from which flight going to happen',
    Destination          varchar(3)                          not null comment 'IATA airport code where arrival is',
    Origin_Price         decimal(10, 2)                      not null comment 'Price of the flight from the origin point of view',
    Origin_Currency      varchar(3)                          not null comment 'Currency in which price is from origin',
    Origin_Date          datetime                            not null comment 'Date of the departure',
    Destination_Price    decimal(10, 2)                      not null comment 'Price of the flight from the destination point of view',
    Destination_Currency varchar(3)                          not null comment 'Currency in which price is from destination',
    Destination_Date     datetime                            not null comment 'Date of the flight back',
    UpdateNo             int                                 not null comment 'Indicate batch number in which record was created',
    ID                   int auto_increment
        primary key
)
    comment 'Stores all available pairs of flights. Date difference between departure and arrival is maximum 14 days.';

create table OneWayFares
(
    Timestamp   timestamp default CURRENT_TIMESTAMP null,
    Origin      varchar(5)                          not null comment 'IATA airport code',
    Destination varchar(5)                          not null comment 'IATA airport code',
    Content     json                                not null comment 'Response from Ryanair API',
    UpdateNo    int       default 0                 not null,
    ID          int auto_increment
        primary key
);

create definer = root@`%` view View_CheapestFlightsInMonth as
select year(`CheapFlights`.`View_FlightPrices`.`Day`)     AS `Year`,
       month(`CheapFlights`.`View_FlightPrices`.`Day`)    AS `Month`,
       `CheapFlights`.`View_FlightPrices`.`Origin`        AS `Origin`,
       `CheapFlights`.`View_FlightPrices`.`Destination`   AS `Destination`,
       min(`CheapFlights`.`View_FlightPrices`.`Price`)    AS `CheapestPrice`,
       `CheapFlights`.`View_FlightPrices`.`Currency`      AS `Currency`,
       min(`CheapFlights`.`View_FlightPrices`.`Day`)      AS `Date`,
       min(`CheapFlights`.`View_FlightPrices`.`UpdateNo`) AS `UpdateNo`
from `CheapFlights`.`View_FlightPrices`
where (`CheapFlights`.`View_FlightPrices`.`UpdateNo` =
       (select max(`CheapFlights`.`View_FlightPrices`.`UpdateNo`) from `CheapFlights`.`View_FlightPrices`))
group by year(`CheapFlights`.`View_FlightPrices`.`Day`), month(`CheapFlights`.`View_FlightPrices`.`Day`),
         `CheapFlights`.`View_FlightPrices`.`Origin`, `CheapFlights`.`View_FlightPrices`.`Destination`,
         `CheapFlights`.`View_FlightPrices`.`Currency`;

-- comment on column View_CheapestFlightsInMonth.Origin not supported: IATA airport code

-- comment on column View_CheapestFlightsInMonth.Destination not supported: IATA airport code

create definer = root@`%` view View_FlightPrices as
select `t`.`Origin`        AS `Origin`,
       `t`.`Destination`   AS `Destination`,
       `t`.`UpdateNo`      AS `UpdateNo`,
       `jt`.`day`          AS `Day`,
       `jt`.`price`        AS `Price`,
       `jt`.`currencyCode` AS `Currency`
from (`CheapFlights`.`OneWayFares` `t` join json_table(`t`.`Content`, '$.outbound.fares[*]'
                                                       columns (`day` date path '$.day', `price` decimal(10, 2) path '$.price.value', `currencyCode` varchar(3) character set utf8mb4 path '$.price.currencyCode')) `jt`
      on (((1 = 1) and (`jt`.`price` is not null))));

-- comment on column View_FlightPrices.Origin not supported: IATA airport code

-- comment on column View_FlightPrices.Destination not supported: IATA airport code

