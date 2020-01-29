using System;

namespace LetsEncryptClient.Model
{
    /// <summary>
    /// Many model objects / messages contain a URI property 'location'
    /// which is associated with the response. Try to abstract this
    /// </summary>
    public interface IHasLocation
    {
        Uri Location { get; set; }
    }
}
