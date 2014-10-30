using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;
using Pirate.Util.Logging;

namespace RegistrationService
{
  public class UpdateState
  {
    public UpdateOperation Operation { get; set; }

    public LdapObject Current { get; set; }
  }

  public enum UpdateOperation
  {
    None,
    Update,
    Create,
    Delete
  }
}