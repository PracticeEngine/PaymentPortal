# Payment Portal

Payment Portal is a front-end website for collecting Fees through a third party provider.  At present we have only built a connection with QuickFee, although the integration is built and available to plug in any provider.  We would love to see the community build more provider interfaces.

## Basic Installation

1. Package the Web Deploy (build the web project and package to a folder)
1. Update the SetParameters.xml in the packaged deployment
    a. Set the connection string for your environment
    b. Set the Web Application Name to your desired IIS location
1. Deploy the web application to IIS
1. Deploy the files in the [/sql/PaymentPortal](./sql/PaymentPortal) folder to your PE database

### To Install Quick Fee Payment Provider

1. Deploy the files in the [/sql/QuickFee](./sql/QuickFee) folder to your PE database
1. Update the deployed quickfee_pe6_fee_bulk_print SP to match your installed URL
1. Update your Web Application Settings
    a. ProviderType setting to 'PEPaymentProvider.QuickFeeProvider'
    b. Set the QuickFee connection details QuickFeeUrl, QuickFeeUsername, QuickFeePassword (obtain these from QuickFee)
    c. Set the FirmName to your firm's display name
    d. Set the logo to a logo file URL for your firm
    e. Pick one of the available professional themes that closely resembles your firm (cerulean, cosmo, darkly, flatly, paper, sandstone, simplex, superhero, united, and yeti) are included from [bootswatch.com](http://bootswatch.com)
    f. Update your Invoice Layout(s) as desired to include the link to QuickFee URL