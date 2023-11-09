namespace MotoGP.Configuration;

public class LocalRepositorySettings
{
    public Uri Directory { get; set; }

    public bool Overwrite { get; set; } = false;

    public bool OverwriteOnError { get; set; } = true;
}