create table OneWayFares
(
    Timestamp   timestamp default CURRENT_TIMESTAMP null,
    Origin      varchar(5)                          not null comment 'IATA airport code',
    Destination varchar(5)                          not null comment 'IATA airport code',
    Content     json                                not null comment 'Response from Ryanair API',
    UpdateNo    int      default 0	            not null 
);
