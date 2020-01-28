using System;

namespace LetsEncryptClient
{
    public interface IHasLocation
    {
        Uri Location { get; set; }
    }
}
