SELECT
    c.ClientId,
    c.ClientName,
    c.ClientEmail,
    c.Locator,
    r.ReservationType,
    r.ReservationDate,
    co.Amount,
    co.Currency
FROM model.dbo.Clients c
INNER JOIN msdb.dbo.Reservations r
    ON c.Locator = r.LocatorId
INNER JOIN tempdb.dbo.Costs co
    ON c.Locator = co.Locator
ORDER BY c.ClientId;




