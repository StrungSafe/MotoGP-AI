﻿namespace MotoGP.Scraper;

public interface IDataWriter
{
    Task SaveData(IEnumerable<Season> seasons);
}